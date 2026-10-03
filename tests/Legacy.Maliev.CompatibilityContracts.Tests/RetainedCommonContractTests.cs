using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using Maliev.MessagingContracts.Contracts.Iam;

namespace Legacy.Maliev.CompatibilityContracts.Tests;

public sealed class RetainedCommonContractTests
{
    private static Type RetainedType(string name)
    {
        var type = Assert.IsAssignableFrom<Type>(typeof(PermissionRegistrationRequest).Assembly.GetType(name));
        Assert.True(type.IsPublic, $"Retained CLR contract {name} must be public.");
        return type;
    }

    [Fact]
    public void PublishedDocumentation_RetainsSharedSourceTypeAndMemberDescriptions()
    {
        var path = Path.ChangeExtension(typeof(PermissionRegistrationRequest).Assembly.Location, ".xml");
        Assert.True(File.Exists(path), "Retained public source documentation must accompany the assembly.");
        var document = XDocument.Load(path);
        var members = document.Descendants("member").ToDictionary(
            member => Assert.IsType<string>(member.Attribute("name")?.Value), StringComparer.Ordinal);
        foreach (var name in new[]
        {
            "T:Maliev.Common.SocialNetworks",
            "P:Maliev.Common.SocialNetworks.WhatsApp",
            "T:Maliev.Common.SupportedFileClass",
            "P:Maliev.Common.SupportedFileClass.SupportedCadFormat",
            "T:Maliev.Common.Enumerations.Severity",
            "T:Maliev.Common.Enumerations.TravelerSortType",
            "T:Maliev.AspNetCore.DataAnnotations.BooleanRequiredAttribute",
            "M:Maliev.AspNetCore.DataAnnotations.BooleanRequiredAttribute.IsValid(System.Object)",
        })
        {
            Assert.True(members.TryGetValue(name, out var member), $"Missing retained XML member {name}");
            Assert.NotNull(member);
            Assert.False(string.IsNullOrWhiteSpace(member.Element("summary")?.Value));
        }
    }

    [Fact]
    public void SocialLinks_RetainExactStaticGetterSurfaceAndDestinations()
    {
        var expected = new Dictionary<string, string>
        {
            ["Facebook"] = "https://www.facebook.com/maliev.manufacturing/",
            ["GoogleMaps"] = "https://goo.gl/maps/AXqLnqUu5dM2",
            ["Instagram"] = "https://www.instagram.com/maliev.manufacturing/",
            ["Threads"] = "https://www.threads.com/@maliev.manufacturing",
            ["Line"] = "https://line.me/ti/p/@maliev",
            ["LinkedIn"] = "https://www.linkedin.com/",
            ["MessengerChat"] = "https://m.me/maliev.manufacturing/",
            ["WhatsApp"] = "https://wa.me/66898950690",
            ["TikTok"] = "https://www.tiktok.com/@maliev.marketing",
            ["YouTube"] = "https://www.youtube.com/channel/UCCosquPSUed6UPlMcRCq0Ig",
        };
        var type = RetainedType("Maliev.Common.SocialNetworks");
        Assert.True(type.IsAbstract && type.IsSealed);
        Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.Static));
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
        Assert.Equal(expected.Keys.Order(StringComparer.Ordinal), properties.Select(p => p.Name).Order(StringComparer.Ordinal));
        foreach (var property in properties)
        {
            Assert.Equal(typeof(string), property.PropertyType);
            Assert.True(property.CanRead);
            Assert.False(property.CanWrite);
            Assert.Equal(expected[property.Name], property.GetValue(null));
        }
    }

    [Theory]
    [InlineData("Maliev.Common.Enumerations.Severity", "Information,Warning,Error")]
    [InlineData("Maliev.Common.Enumerations.TravelerSortType", "TravelerId_Ascending,TravelerId_Descending,TravelerOrderId_Ascending,TravelerOrderId_Descending,TravelerPromisedDate_Ascending,TravelerPromisedDate_Descending,TravelerManufactured_Ascending,TravelerManufactured_Descending,TravelerTargetQuantity_Ascending,TravelerTargetQuantity_Descending,TravelerCreatedDate_Ascending,TravelerCreatedDate_Descending,TravelerModifiedDate_Ascending,TravelerModifiedDate_Descending")]
    public void Enumerations_RetainNamesOrdinalsAndNumericJson(string name, string names)
    {
        var type = RetainedType(name);
        Assert.True(type.IsEnum);
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(type));
        Assert.Equal(names.Split(','), Enum.GetNames(type));
        var values = Enum.GetValues(type);
        for (var index = 0; index < values.Length; index++)
        {
            var value = values.GetValue(index);
            Assert.Equal(index, Convert.ToInt32(value));
            var json = JsonSerializer.Serialize(value, type);
            Assert.Equal(index.ToString(System.Globalization.CultureInfo.InvariantCulture), json);
            Assert.Equal(value, JsonSerializer.Deserialize(json, type));
        }
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(null, false)]
    [InlineData("true", false)]
    [InlineData("false", false)]
    [InlineData(0, false)]
    [InlineData(1, false)]
    public void BooleanRequired_RequiresBooleanTypeAndAcceptsBothValues(object? input, bool expected)
    {
        var type = RetainedType("Maliev.AspNetCore.DataAnnotations.BooleanRequiredAttribute");
        Assert.Equal(typeof(RequiredAttribute), type.BaseType);
        Assert.False(type.IsSealed);
        var attribute = Assert.IsAssignableFrom<ValidationAttribute>(Activator.CreateInstance(type));
        Assert.Equal(expected, attribute.IsValid(input));
        var context = new ValidationContext(new object()) { MemberName = "Consent" };
        var result = attribute.GetValidationResult(input, context);
        if (expected)
        {
            Assert.Null(result);
        }
        else
        {
            Assert.NotNull(result);
            Assert.Contains("Consent", result.MemberNames);
        }
    }

    [Fact]
    public void FileRegistry_RetainsEveryGroupExtensionAndMutablePropertyContract()
    {
        var expected = new Dictionary<string, Dictionary<string, List<string>>>
        {
            ["Supported3DPrintFormat"] = new()
            {
                ["3D Models"] = [".obj", ".3mf"],
                ["STEP Files"] = [".stp", ".step"],
                ["StereoLithography Files"] = [".stl"],
            },
            ["SupportedDocumentFormat"] = new()
            {
                ["Documentation"] = [".pdf", ".tiff", ".dxf", ".dwg"],
                ["Images Files"] = [".jpg", ".jpeg", ".png"],
            },
            ["SupportedCadFormat"] = new()
            {
                ["IGES Files"] = [".igs", ".iges"],
                ["AutoCAD Files"] = [".dwg", ".dxf", ".dwf", ".dwfx"],
                ["Parasolid Files"] = [".x_t", ".x_b", ".xmt_txt"],
                ["ProE/Creo Files"] = [".prt", ".asm", ".prt.*", ".asm.*"],
                ["ACIS Kernel SAT Files"] = [".sat", ".sab"],
                ["STEP Files"] = [".stp", ".step"],
                ["VDA Files"] = [".vda"],
                ["Rhino 3D Files"] = [".3dm"],
                ["SolidWorks Files"] = [".sldprt", ".sldasm", ".slddrw"],
                ["Solid Edge Files"] = [".par", ".psm", ".asm"],
                ["Autodesk Inventor Files"] = [".ipt", ".iam", ".idw"],
                ["KeyCreator Files"] = [".ckd"],
                ["Unigraphics/NX Files"] = [".prt"],
                ["StereoLithography Files"] = [".stl"],
                ["CATIA Files"] = [".model", ".exp", ".catpart", ".catproduct"],
                ["SpaceClaim Files"] = [".scdoc"],
                ["Alibre/Geomagic Design Files"] = [".ad_prt", ".ad_smp"],
                ["HPGL Plotter Files"] = [".plt"],
                ["PostScript Files"] = [".eps", ".ai", ".ps"],
                ["Images Files"] = [".jpg", ".jpeg", ".png"],
            },
        };
        var type = RetainedType("Maliev.Common.SupportedFileClass");
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
        Assert.Equal(expected.Keys.Order(StringComparer.Ordinal), properties.Select(p => p.Name).Order(StringComparer.Ordinal));
        foreach (var property in properties)
        {
            Assert.Equal(typeof(Dictionary<string, List<string>>), property.PropertyType);
            Assert.True(property.CanRead && property.CanWrite);
            var original = Assert.IsType<Dictionary<string, List<string>>>(property.GetValue(null));
            Assert.Equal(expected[property.Name].Keys, original.Keys);
            foreach (var group in expected[property.Name])
            {
                Assert.Equal(group.Value, original[group.Key]);
            }
            var replacement = new Dictionary<string, List<string>> { ["Fixture"] = [".fixture"] };
            try
            {
                property.SetValue(null, replacement);
                Assert.Same(replacement, property.GetValue(null));
                var json = JsonSerializer.Serialize(property.GetValue(null), property.PropertyType);
                var roundTrip = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                Assert.NotNull(roundTrip);
                Assert.Equal([".fixture"], roundTrip["Fixture"]);
            }
            finally
            {
                property.SetValue(null, original);
            }
        }
    }
}
