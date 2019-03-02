﻿using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Framework;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Generic;

namespace Hackathon.Feature.DynamicDelete.Commands
{
    [Serializable]
    public class DeleteSelected : Command
    {
        private Sitecore.Data.Database _master =
                    Sitecore.Configuration.Factory.GetDatabase("master");
        /// <summary>
        /// Executes the command in the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Execute(CommandContext context)
        {
            System.Web.HttpContext itemContext = System.Web.HttpContext.Current;

            // string sc_selectedItems = "{1BAB6C8F-6442-4A8E-867B-725C6A4C98F8},{CD3EAF80-AE0D-460C-91B4-BDBF9FD88340}"; 
            string sc_selectedItems = itemContext.Request.Cookies["sc_selectedItems"].Value;

            var itemIDs = sc_selectedItems.Split(',');
            if (string.IsNullOrEmpty(sc_selectedItems))
            {
                SheerResponse.Alert("No items have been selected, select items using the check box then retry Delete selected items", Array.Empty<string>());
                return;
            }

            List<Item> itemsList = new List<Item>();
            foreach (var itemID in itemIDs)
            {
                //Return Item from Sitecore 

                Item contextItem = _master.GetItem(ID.Parse(itemID));
                itemsList.Add(contextItem);
            }
            Items.Delete(itemsList.ToArray());
        }

        /// <summary>
        /// Queries the state of the command.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The state of the command.</returns>
        public override CommandState QueryState(CommandContext context)
        {
            Error.AssertObject(context, "context");
            if (context.Items.Length == 0)
            {
                return CommandState.Disabled;
            }
            Item[] items = context.Items;
            for (int i = 0; i < items.Length; i++)
            {
                Item item = items[i];
                if (!item.Access.CanDelete())
                {
                    return CommandState.Disabled;
                }
                if (item.Appearance.ReadOnly)
                {
                    return CommandState.Disabled;
                }
                if (Command.IsLockedByOther(item))
                {
                    return CommandState.Disabled;
                }
            }
            return base.QueryState(context);
        }
    }
}
