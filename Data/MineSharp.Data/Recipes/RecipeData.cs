﻿using MineSharp.Core.Common.Items;
using MineSharp.Core.Common.Recipes;
using MineSharp.Data.Framework;
using MineSharp.Data.Internal;

namespace MineSharp.Data.Recipes;

internal class RecipeData(RecipeProvider provider) : IndexedData<RecipeDataBlob>(provider), IRecipeData
{
    private Dictionary<ItemType, Recipe[]> recipes = new();

    public Recipe[]? ByItem(ItemType type)
    {
        if (!Loaded)
        {
            Load();
        }

        return recipes!.GetValueOrDefault(type, null);
    }

    protected override void InitializeData(RecipeDataBlob data)
    {
        recipes = data.Recipes;
    }
}
