namespace AStockQuant.Domain.Entities;

public sealed class Stock
{
	public Stock(int id, string code, string name, string exchangeCode, DateOnly? listingDate, bool isActive = true)
	{
		if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Stock code is required.", nameof(code));
		if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Stock name is required.", nameof(name));
		Id = id; Code = code.Trim(); Name = name.Trim(); ExchangeCode = exchangeCode.Trim(); ListingDate = listingDate; IsActive = isActive;
	}
	public int Id { get; }
	public string Code { get; }
	public string Name { get; }
	public string ExchangeCode { get; }
	public DateOnly? ListingDate { get; }
	public bool IsActive { get; }
}