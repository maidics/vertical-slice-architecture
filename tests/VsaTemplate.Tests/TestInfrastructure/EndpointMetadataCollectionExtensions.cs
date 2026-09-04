using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace VsaTemplate.Tests.TestInfrastructure;

public static class EndpointMetadataCollectionExtensions
{
    extension(EndpointMetadataCollection metadata)
    {
        public void ShouldHaveEndpointName(string value)
        {
            var name = metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName;
            name.ShouldNotBeNull();
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
            auth[0].Roles.ShouldBe(string.Join(",", roles));
        }
    }
}
