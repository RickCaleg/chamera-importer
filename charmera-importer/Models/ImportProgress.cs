namespace charmera_importer.Models;

public sealed record ImportProgress(int Completed, int Total, string CurrentFileName);
