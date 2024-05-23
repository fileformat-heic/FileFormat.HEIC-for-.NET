/*
 * Openize.IsoBmff
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of Openize.IsoBmff.
 *
 * Openize.IsoBmff is available under MIT license, which is
 * available along with Openize.IsoBmff sources.
 */

using Openize.IsoBmff.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Openize.IsoBmff
{
    /// <summary>
    /// The ItemPropertiesBox enables the association of any item with an ordered set of item properties.
    /// Item properties are small data records.
    /// </summary>
    public class ItemPropertiesBox : Box
    {
        /// <summary>
        /// Contains an implicitly indexed list of item properties.
        /// </summary>
        ItemPropertyContainerBox property_container;

        /// <summary>
        /// Associates items with item properties.
        /// </summary>
        List<ItemPropertyAssociation> association;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type}";

        /// <summary>
        /// Observable collection of the nested boxes.
        /// </summary>
        public ObservableCollection<Box> Children { get; set; }

        /// <summary>
        /// Create the box object from the bitstream, box size and start position.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        /// <param name="startPos">Start position in bits.</param>
        public ItemPropertiesBox(BitStreamReader stream, ulong size, ulong startPos) : base (BoxType.iprp, size)
        {
            property_container = new ItemPropertyContainerBox(stream, stream.GetBitPosition());

            Children = new ObservableCollection<Box>
            {
                property_container
            };

            association = new List<ItemPropertyAssociation>();
            while (stream.GetBitPosition() < startPos + size * 8)
            {
                var impa = new ItemPropertyAssociation(stream);
                association.Add(impa);
                Children.Add(impa);
            }
        }

        /// <summary>
        /// Returns properties in a convinient form factor.
        /// </summary>
        /// <returns>Dictionary with Lists of boxes that can be accessed by item id.</returns>
        public Dictionary<uint, List<Box>> GetProperties()
        {
            var properties = new Dictionary<uint, List<Box>>();
            var essentialProperties = new Dictionary<uint, List<Box>>();
            var nonEssentialProperties = new Dictionary<uint, List<Box>>();

            foreach (var item in association) {
                foreach (var itemProperty in item.entries)
                {
                    var all = new List<Box>();
                    var essential = new List<Box>();
                    var nonEssential = new List<Box>();

                    foreach (var prop in itemProperty.associations)
                    {
                        all.Add(property_container.GetPropertyByIndex(prop.property_index));
                        if (prop.essential)
                            essential.Add(property_container.GetPropertyByIndex(prop.property_index));
                        else
                            nonEssential.Add(property_container.GetPropertyByIndex(prop.property_index));
                    }

                    properties.Add(itemProperty.item_ID, all);
                    essentialProperties.Add(itemProperty.item_ID, essential);
                    nonEssentialProperties.Add(itemProperty.item_ID, nonEssential);
                }
            }

            return properties;
        }
    }
}
