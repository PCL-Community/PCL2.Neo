using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Service.Accounts.Storage
{
    // Decorate with JsonPolymorphic and JsonDerivedType for System.Text.Json
    // We'll use "UserType" as the discriminator property.
    // By default, System.Text.Json uses "$type" if not specified, but UserType is more natural here.
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "StorageType")]
    [JsonDerivedType(typeof(MsaAccount), typeDiscriminator: UserTypeConstants.Msa)]
    [JsonDerivedType(typeof(YggdrasilAccount), typeDiscriminator: UserTypeConstants.Yggdrasil)]
    [JsonDerivedType(typeof(OfflineAccount), typeDiscriminator: UserTypeConstants.Offline)]
    public abstract record BaseAccount(string UserType)
    {
        // Properties common to all account types
        public required string Uuid { get; set; } // Made set for flexibility, could be init
        public required string UserName { get; init; }
        public string UserType { get; } = UserType; // This is crucial for discrimination
        public string UserProperties { get; init; } = string.Empty;
        public required List<Skin> Skins { get; init; } = [];
        public required List<Cape> Capes { get; init; } = [];

        // Protected constructor to ensure UserType is set by derived classes
        // Initialize lists to prevent null reference exceptions if not provided by derived class (though 'required' helps)
    }
}