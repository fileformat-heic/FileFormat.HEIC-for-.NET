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

namespace Openize.IsoBmff
{
    /// <summary>
    /// The EntityToGroupBox specifies an entity group.
    /// </summary>
    public class EntityToGroupBox : FullBox
    {
        /// <summary>
        /// A non-negative integer assigned to the particular grouping that shall not be equal
        /// to any group_id value of any other EntityToGroupBox, any item_ID value of the hierarchy
        /// level(file, movie. or track) that contains the GroupsListBox, or any track_ID value(when the
        /// GroupsListBox is contained in the file level).
        /// </summary>
        public uint group_id;

        /// <summary>
        /// The number of entity_id values mapped to this entity group.
        /// </summary>
        public uint num_entities_in_group;

        /// <summary>
        /// Array of identificators of items that are present in the hierarchy level(file, movie or track) 
        /// that contains the GroupsListBox, or to a track, when a track with track_ID equal to entity_id 
        /// is present and the GroupsListBox is contained in the file level.
        /// </summary>
        public uint[] entities;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString
        {
            get
            {
                var str = "";
                foreach (var id in entities)
                    str += " " + id;
                return $"{type} group: {group_id} count: {num_entities_in_group} {str}";
            }
        }

        /// <summary>
        /// Create the box object from the bitstream.
        /// </summary>
        /// <param name="stream">File stream.</param>
        public EntityToGroupBox(BitStreamReader stream) : base (stream)
        {
            group_id = (uint)stream.Read(32);
            num_entities_in_group = (uint)stream.Read(32);
            entities = new uint[num_entities_in_group];

            for (int i = 0; i < num_entities_in_group; i++)
                entities[i] = (uint)stream.Read(32);
        }
    }
}
