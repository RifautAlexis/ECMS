using ECMS.Contracts.Configuration;

namespace ECMS.Host.Configuration.Models;

public record ConfigurationSettings(
    CollectionConfiguration Collection,
    CoreConfiguration Core,
    CommunicationConfiguration Communication
);
