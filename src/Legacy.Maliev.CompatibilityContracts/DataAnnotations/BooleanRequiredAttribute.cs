// <copyright file="BooleanRequiredAttribute.cs" company="Maliev Company Limited">
// Copyright (c) Maliev Company Limited. All rights reserved.
// </copyright>

namespace Maliev.AspNetCore.DataAnnotations
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Boolean Required.
    /// </summary>
    /// <seealso cref="System.ComponentModel.DataAnnotations.RequiredAttribute" />
    public class BooleanRequiredAttribute : RequiredAttribute
    {
        /// <summary>
        /// Returns true if value is valid.
        /// </summary>
        /// <param name="value">The data field value to validate.</param>
        /// <returns>
        /// true if validation is successful; otherwise, false.
        /// </returns>
        public override bool IsValid(object? value)
        {
            return value is bool;
        }
    }
}
