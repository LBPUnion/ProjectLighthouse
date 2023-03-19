namespace LBPUnion.ProjectLighthouse.Types.Serialization;

/// <summary>
/// Allows serializable classes to fetch other data using DI services
/// Function is called using reflection so there is no required methods.
/// Method signature: public async Task PrepareSerialization(list of services)
/// </summary>
public interface INeedsPreparationForSerialization { }