// src/EventManagement.Domain/SmartEnums/EventStatus.cs

using EventManagement.Domain.Common;

namespace EventManagement.Domain.SmartEnums;

public sealed class EventStatus
{
    public string Code { get; }
    public string Name { get; }

    private EventStatus(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public static readonly EventStatus Draft = new("draft", "Brouillon");
    public static readonly EventStatus Confirmed = new("confirmed", "Confirmé");
    public static readonly EventStatus Cancelled = new("cancelled", "Annulé");
    public static readonly EventStatus Closed = new("closed", "Clôturé");

    private static readonly HashSet<(EventStatus, EventStatus)> AllowedTransitions = new()
    {
    (Draft, Confirmed),
    (Draft, Cancelled),
    (Confirmed, Closed),
    (Confirmed, Cancelled)
    };

    public bool CanTransitionTo(EventStatus target)
    {
        return AllowedTransitions.Contains((this, target));
    }

    public static EventStatus FromCode(string code)
    {
        return code switch
        {
            "draft" => Draft,
            "confirmed" => Confirmed,
            "cancelled" => Cancelled,
            "closed" => Closed,
            _ => throw new DomainException($"Statut inconnu: {code}")
        };
    }

    public override bool Equals(object? obj)
    {
        return obj is EventStatus other && other.Code == Code;
    }

    public override int GetHashCode()
    {
        return Code.GetHashCode();
    }
}