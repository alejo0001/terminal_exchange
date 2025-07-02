using System;
using CrmAPI.Application.Common.Dtos;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.BusinessAlgorithms;

public static class ProductPricingAlgorithms
{
    /// <summary>
    ///     Obtains <b>Scholarship</b> discount data from provided sources in priority order according to argument
    ///     positions.
    /// </summary>
    /// <param name="contactLead"></param>
    /// <param name="courseImportedTlmk"></param>
    /// <param name="sanitizeDiscountPercentDlgt">
    ///     <para>
    ///         Although very unlikely, it can happen that business data is bad or because of some other unforeseen
    ///         circumstances, source data might not provide any discount data -- for <b>Scholarship</b> it's mandatory.
    ///         Therefore, it is up too caller/business use case to decide, how to handle this (hopefully rare)
    ///         condition, e.g. what the fallback percentage needs to be.
    ///     </para>
    ///     <para>
    ///         As a bonus, this delegate allows to override "Discount &amp; The Rest" calculation.
    ///     </para>
    /// </param>
    /// <returns>Tuple of "Discount &amp; The Rest"</returns>
    /// <remarks>Applies "Ceiling" for obtained and sanitized Discount.</remarks>
    /// <code>
    /// finalDiscount = sanitizeDiscountPercentDlgt(fromDisCountSources)
    /// theRest = 100 - finalDiscount
    /// </code>
    /// <exception cref="ArgumentNullException">
    ///     When <paramref name="sanitizeDiscountPercentDlgt" /> is null, because it's there for business rule
    ///     enforcement.
    /// </exception>
    [MustUseReturnValue("Crucial data form customer communication & sales standpoint.")]
    public static DiscountPercent GetScholarshipDiscountPercent(
        ContactLead? contactLead,
        CourseImportedTlmkDto? courseImportedTlmk,
        Func<decimal?, decimal> sanitizeDiscountPercentDlgt)
    {
        ArgumentNullException.ThrowIfNull(sanitizeDiscountPercentDlgt);

        var discount = contactLead is { Discount: > 0 }
            ? contactLead.Discount
            : GetTlmkDiscount();

        var finalDiscount = sanitizeDiscountPercentDlgt(discount);
        finalDiscount = Math.Ceiling(finalDiscount);

        return new(finalDiscount);

        // Returns null, when no usable data provided.
        decimal? GetTlmkDiscount() =>
            courseImportedTlmk?.ScholarshipPercent switch
            {
                { } sp and > 0 => 100 - sp,
                _ => null,
            };
    }

    /// <summary>
    ///     Presents Discount percentage as <c>1.00%, 10.00%, 100.00%</c>
    /// </summary>
    /// <param name="Discount">Discount value.</param>
    public readonly record struct DiscountPercent(decimal Discount)
    {
        /// <summary>
        ///     Computed as <c>100 - Discount</c>.
        /// </summary>
        public decimal TheRest => 100 - Discount;

        public void Deconstruct(out decimal discount, out decimal theRest)
        {
            discount = Discount;
            theRest = TheRest;
        }
    }
}
