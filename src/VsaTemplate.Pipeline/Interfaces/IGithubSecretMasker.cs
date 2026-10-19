namespace VsaTemplate.Pipeline.Interfaces;

public interface IGithubSecretMasker
{
    void Mask(string secret);
}
