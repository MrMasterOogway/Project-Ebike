<Query Kind="Program">
  <Connection>
    <ID>3b1ca1a7-bed3-4b4a-8cc9-3f492420734a</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <Server>.</Server>
    <Database>eBike_2025</Database>
    <DisplayName>eBike_2025-EF</DisplayName>
    <DriverData>
      <EncryptSqlTraffic>True</EncryptSqlTraffic>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
    </DriverData>
  </Connection>
</Query>

void Main()
{

	Console.WriteLine(GetPurchaseOrder(125));

}

#region Test Methods


#endregion

#region Support Methods
public Exception GetInnerException(System.Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}
#endregion

#region Methods
public List<VendorsView> GetVendorList()
{
	#region business rules



	#endregion

	#region Query
	//Drop down menu list
	return Vendors
		.Select(x => new VendorsView
		{
			VendorID = x.VendorID,
			VendorName = x.VendorName,
			Address = x.Address,
			Phone = x.Phone
		})
		.ToList();

	#endregion
}

public List<PartsView> GetParts(int vendorID)
{
	#region business rules



	#endregion

	#region Query
	//selected vendor information
	return Parts
		.Where(x => x.VendorID == vendorID)
		.Select(x => new PartsView
		{
			PartsID = x.PartID,
			Description = x.Description,
			QOH = x.QuantityOnHand,
			ROL = x.ReorderLevel,
			QOO = x.QuantityOnOrder,
			QTO = x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder),
			PurchasePrice = x.PurchasePrice
		}).ToList();


	#endregion
}

public PurchaseOrderView GetPurchaseOrder(int purchseOrderNumber)
{
	#region business rules


	#endregion

	#region Query
	//selected vendor information
	return PurchaseOrders.Where(x => x.PurchaseOrderNumber == purchseOrderNumber)
	.Select(x => new PurchaseOrderView
	{
		PurchaseOrderID = x.PurchaseOrderID,
		
		SubTotal = x.SubTotal,
		GST = x.TaxAmount,
		Total = x.SubTotal + x.TaxAmount,
		VendorID = x.VendorID,
		VendorName = x.Vendor.VendorName,
		Address = x.Vendor.Address,
		Phone = x.Vendor.Phone,
		PONumber = x.PurchaseOrderNumber,
		PurchaseOrderDetails = PurchaseOrderDetails
			.Where(p => p.PurchaseOrderID == x.PurchaseOrderID)
			.Select(p => new PurchaseOrderDetailsView
			{
				PurchasOrderDetailID = p.PurchaseOrderDetailID,
				PurchaseOrderID = p.PurchaseOrderID,
				PartID = p.PartID,
				Quantity = p.Quantity,
				PurchasePrice = p.PurchasePrice,
				VendorPartNumber = p.VendorPartNumber,
				Part = Parts
					.Where(pa => pa.PartID == p.PartID)
					.Select(pa => new PartsView
					{
						PartsID = pa.PartID,
						Description = pa.Description,
						QOH = pa.QuantityOnHand,
						ROL = pa.ReorderLevel,
						QOO = pa.QuantityOnOrder,
						QTO = pa.ReorderLevel - (pa.QuantityOnHand + pa.QuantityOnOrder),
						PurchasePrice = pa.PurchasePrice,
						VendorID = pa.VendorID
					}).FirstOrDefault()
			}).ToList()
	}).FirstOrDefault();

	#endregion
}

public PurchaseOrderView AddEditPurchaseOrder(PurchaseOrderView orderView)
{


	#region Query Select

	PurchaseOrder order = PurchaseOrders
		.Where(x => x.PurchaseOrderID == orderView.PurchaseOrderID)
		.FirstOrDefault();

	if (order == null)
	{
		order = new PurchaseOrder();
		//Do i need to set the PurchaseOrderID here or is it auto?
		
		//Sets PO number if PO is new.
		int maxID = PurchaseOrders.Where(x => x.VendorID == orderView.VendorID).Any() 
					? PurchaseOrders.Max(x => x.PurchaseOrderNumber) + 1 : 1;
		//Sets new PO num
		order.PurchaseOrderNumber = maxID;
	}
	
	order.OrderDate = orderView.OrderDate;
	order.TaxAmount = orderView.GST;
	order.SubTotal = orderView.SubTotal;
	order.Closed = false;

	//TODO: Is the default null or do i need to add this to the view model?
	//order.Notes = orderView.Notes;

	//TODO FIX EMPLOYEEID to = user logged in?
	//order.EmployeeID = orderView.EmployeeID;
	
	order.VendorID = orderView.VendorID;
	order.RemoveFromViewFlag = false;

	foreach (var orderDetailsView in orderView.PurchaseOrderDetails)
	{
		PurchaseOrderDetail orderDetail = PurchaseOrderDetails
						.Where(x => x.PurchaseOrderID == orderView.PurchaseOrderID)
						.FirstOrDefault();

		if (orderDetail == null)
		{
			orderDetail = new PurchaseOrderDetail();
		}
		orderDetail.PartID = orderDetailsView.PartID;
		orderDetail.Quantity = orderDetailsView.Quantity;
		orderDetail.PurchasePrice = orderDetailsView.PurchasePrice;
		orderDetail.VendorPartNumber = orderDetailsView.VendorPartNumber;
	}

	return GetPurchaseOrder(order.PurchaseOrderID);

	#endregion
}

#endregion
#region View Models

public class CategoryView
{
	public int CategoryID { get; set; }
	public string Description { get; set; }
	public bool RemoveFromViewFlag { get; set; }
	public List<PartsView> Parts { get; set; }
}

public class VendorsView
{
	public int VendorID { get; set; }
	public string VendorName { get; set; }
	public string Phone { get; set; }
	public string Address { get; set; }
	public string City { get; set; }
	public string ProvinceID { get; set; }
	public string PostalCode { get; set; }
	public List<PartsView> Parts { get; set; }
	public List<PurchaseOrderView> PurchaseOrders { get; set; }
}

public class PurchaseOrderView
{
	public int PurchaseOrderID { get; set; }
	//cost for order 
	public decimal SubTotal { get; set; }
	public decimal GST { get; set; }
	public decimal Total { get; set; }
	public DateTime OrderDate { get; set; }
	//vendor info for display on page
	public int VendorID { get; set; }
	public string VendorName { get; set; }
	public string Address { get; set; }
	public string Phone { get; set; }
	public int PONumber { get; set; } //not in vendor view

	//parts in order
	public List<PurchaseOrderDetailsView> PurchaseOrderDetails { get; set; }
}

public class PurchaseOrderDetailsView
{
	public int PurchasOrderDetailID { get; set; }
	public int PurchaseOrderID { get; set; }
	public int PartID { get; set; }
	public int Quantity { get; set; }
	public decimal PurchasePrice { get; set; }
	public string VendorPartNumber { get; set; }
	public bool RemoveFromViewFlag { get; set; }
	public PartsView Part { get; set; }
}

public class PartsView
{
	public int PartsID { get; set; }
	public string Description { get; set; }
	public int QOH { get; set; } //On Hand
	public int ROL { get; set; } //ReOrder Level
	public int QOO { get; set; } //On Order set by QTO plus QOO
	public int QTO { get; set; } //To Order = ROL - QOO + QOH
	public decimal PurchasePrice { get; set; }
	public int VendorID { get; set; }
}
#endregion

