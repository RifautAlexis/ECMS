namespace ECMS.Collection.Mapping;

public interface IDeviceDataMapper
{
    object Map(int kind, string json);
}
