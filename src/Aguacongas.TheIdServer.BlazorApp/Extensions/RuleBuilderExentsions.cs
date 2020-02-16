﻿using Aguacongas.TheIdServer.BlazorApp.Validators;
using System.Collections.Generic;

namespace FluentValidation
{
    public static class RuleBuilderExentsions
    {
        public static IRuleBuilderOptions<TItem, TProperty> IsUnique<TItem, TProperty>(this IRuleBuilder<TItem, TProperty> ruleBuilder, IEnumerable<TItem> items)
            where TItem : class
        {
            return ruleBuilder.SetValidator(new UniqueValidator<TItem>(items));
        }

    }
}