namespace CommonCore
{
    public interface IFaction : ILocatableService
    {
        FactionDefinition Definition { get; }

        EFactionRelationship GetRelationshipTo(IFaction InOther);
    }
}
