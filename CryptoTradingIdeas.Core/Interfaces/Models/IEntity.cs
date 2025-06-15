namespace CryptoTradingIdeas.Core.Interfaces.Models;

public interface IEntity<out TKey>
{
    TKey Id { get; }
}