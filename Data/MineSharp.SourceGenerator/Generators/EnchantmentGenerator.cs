using MineSharp.SourceGenerator.Utils;
using Newtonsoft.Json.Linq;

namespace MineSharp.SourceGenerator.Generators;

public class EnchantmentGenerator
{
    private readonly Generator typeGenerator =
        new Generator("enchantments", GetName, "EnchantmentType", "Enchantments");

    private readonly Generator categoryGenerator =
        new Generator("enchantments", GetCategoryName, "EnchantmentCategory", "Enchantments");

    public Task Run(MinecraftDataWrapper wrapper)
    {
        return Task.WhenAll(
            typeGenerator.Generate(wrapper),
            categoryGenerator.Generate(wrapper));
    }

    private static string GetName(JToken token)
    {
        var name = (string)token.SelectToken("name")!;
        return NameUtils.GetEnchantmentName(name);
    }

    private static string GetCategoryName(JToken token)
    {
        var name = (string)token.SelectToken("category")!;
        return NameUtils.GetEnchantmentCategory(name);
    }
}
