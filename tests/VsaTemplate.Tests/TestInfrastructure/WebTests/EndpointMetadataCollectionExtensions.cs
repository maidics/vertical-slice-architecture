using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace VsaTemplate.Tests.TestInfrastructure.WebTests;

public static class EndpointMetadataCollectionExtensions
{
    extension(EndpointMetadataCollection metadata)
    {
        public void ShouldHaveEndpointName(string value)
        {
            var name = metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName;
            name.ShouldNotBeNull();
            name.ShouldBe(value);
        }

        public void ShouldNotHaveAuthMetadata()
        {
            var auth = metadata.GetOrderedMetadata<IAuthorizeData>();
            auth.Count.ShouldBe(0);
        }

        public void ShouldHaveOneAuthMetadataWithoutRoles()
        {
            var auth = metadata.GetOrderedMetadata<IAuthorizeData>();
            auth.Count.ShouldBe(1);
            auth[0].Roles.ShouldBeNull();
        }

        public void ShouldHaveOneAuthMetadataWithRoles(params string[] roles)
        {
            ArgumentOutOfRangeException.ThrowIfZero(roles.Length);

            var auth = metadata.GetOrderedMetadata<IAuthorizeData>();
            auth.Count.ShouldBe(1);
            auth[0].Roles.ShouldNotBeNull();

            var applied = auth[0]
                .Roles!.Split(",", StringSplitOptions.RemoveEmptyEntries)
                .ToHashSet();
            applied.ShouldBeEquivalentTo(roles.ToHashSet());
        }
    }
}
