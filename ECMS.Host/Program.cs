using ECMS.Collection;
using ECMS.Contracts;
using ECMS.Contracts.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.SetBasePath(AppContext.BaseDirectory);
builder.Configuration.AddJsonFile("config.json", optional: false);

var collectionConfiguration = builder.Configuration
    .GetSection("collection")
    .Get<CollectionConfiguration>()
    ?? throw new InvalidDataException("The config.json file must contain a collection section.");

// var coreConfiguration = builder.Configuration
//                                      .GetSection("core")
//                                      .Get<CoreConfiguration>()
//                            ?? throw new InvalidDataException("The config.json file must contain a core section.");
//
// var communicationConfiguration = builder.Configuration
//                                      .GetSection("communication")
//                                      .Get<CommunicationConfiguration>()
//                            ?? throw new InvalidDataException("The config.json file must contain a communication section.");

builder.Services.AddDeviceCollection(collectionConfiguration);

var host = builder.Build();
host.Run();