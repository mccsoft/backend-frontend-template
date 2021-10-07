using System.Collections.Generic;

namespace MccSoft.Logging
{
    /// <summary>
    /// Additional operation parameters which don't belong to the operation context.
    /// </summary>
    public class AdditionalParams : Dictionary<Field, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalParams"/> class.
        /// </summary>
        public AdditionalParams() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalParams"/> class
        /// that contains elements copied from the specified collection.
        /// </summary>
        /// <param name="collection">The source of elements for the created object.</param>
        public AdditionalParams(IEnumerable<KeyValuePair<Field, object>> collection)
            : base(collection) { }
    }
}
