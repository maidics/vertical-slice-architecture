using Octokit;

namespace VsaTemplate.Pipeline.Tests.TestInfrastructure.Extensions;

public static class ReleaseExtensions
{
    extension(Release)
    {
        public static Release Create(string tagName = "v1.0.0", bool draft = false, long id = 1) =>
            new(
                url: "",
                htmlUrl: "",
                assetsUrl: "",
                uploadUrl: "",
                id: id,
                nodeId: "",
                tagName: tagName,
                targetCommitish: "main",
                name: tagName,
                body: "",
                draft: draft,
                prerelease: false,
                createdAt: DateTimeOffset.UnixEpoch,
                publishedAt: null,
                author: null!,
                tarballUrl: "",
                zipballUrl: "",
                assets: []
            );
    }
}
