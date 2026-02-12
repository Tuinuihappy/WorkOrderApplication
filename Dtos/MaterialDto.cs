namespace WorkOrderApplication.API.Dtos;

// -------------------- สำหรับ Create --------------------
public record MaterialCreateDto(
    string MaterialNumber,
    string MaterialDescription,
    decimal ReqmntQty,
    decimal QtyWthdrn,
    string BUn,
    string? OpAc,
    string? SortString,
    string? SLoc
);

// -------------------- สำหรับ Update --------------------
public record MaterialUpdateDto(
    int? Id,
    string MaterialNumber,
    string MaterialDescription,
    decimal ReqmntQty,
    decimal QtyWthdrn,
    string BUn,
    string? OpAc,
    string? SortString,
    string? SLoc
);

// -------------------- สำหรับ List / Table View --------------------
public record MaterialListDto(
    int Id,
    string MaterialNumber,
    string MaterialDescription,
    decimal ReqmntQty,
    decimal QtyWthdrn,
    string BUn,
    string? OpAc,
    string? SortString,
    string? SLoc
);

// -------------------- รายละเอียดเต็ม --------------------
public record MaterialDetailsDto(
    int Id,

    string MaterialNumber,
    string MaterialDescription,
    decimal ReqmntQty,
    decimal QtyWthdrn,
    string BUn,
    string? OpAc,
    string? SortString,
    string? SLoc,
    int WorkOrderId,
    string Order
);
