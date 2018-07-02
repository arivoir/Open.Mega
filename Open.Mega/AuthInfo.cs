namespace Open.Mega
{
    public class AuthInfo
    {
        public AuthInfo(string email, string hash, byte[] passwordAesKey)
        {
            this.Email = email;
            this.Hash = hash;
            this.PasswordAesKey = passwordAesKey;
        }

        public string Email { get; private set; }
        public string Hash { get; private set; }
        public byte[] PasswordAesKey { get; private set; }
    }
}
