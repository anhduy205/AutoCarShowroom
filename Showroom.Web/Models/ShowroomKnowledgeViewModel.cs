namespace Showroom.Web.Models;

public class ShowroomKnowledgeViewModel
{
    public AdminDashboardViewModel Dashboard { get; init; } = new();

    public IReadOnlyList<CarListItemViewModel> Cars { get; init; } = Array.Empty<CarListItemViewModel>();
}
