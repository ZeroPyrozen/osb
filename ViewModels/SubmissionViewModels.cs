#nullable enable

using System.ComponentModel.DataAnnotations;
using osb.Data;

namespace osb.ViewModels;

/// <summary>What a member fills in on the submit page. The rules that need osu! are in SubmissionService.</summary>
public class SubmitForm
{
    [Required(ErrorMessage = "Paste a link to the beatmapset, or its ID.")]
    [StringLength(300, ErrorMessage = "That's too long for a beatmapset link.")]
    public string Beatmapset { get; set; } = "";

    [Required(ErrorMessage = "List at least one storyboarder.")]
    [StringLength(1000, ErrorMessage = "That's too long for a list of storyboarders.")]
    public string Storyboarders { get; set; } = "";

    [Required(ErrorMessage = "Say which tool it was made with.")]
    [StringLength(32, ErrorMessage = "Keep the tool's name to 32 characters.")]
    public string Medium { get; set; } = "";

    [StringLength(300, ErrorMessage = "That's too long for a video link.")]
    public string? Video { get; set; }

    /// <summary>Suggested tag slugs.</summary>
    public List<string> Tags { get; set; } = new();

    [StringLength(1000, ErrorMessage = "Keep the note to 1,000 characters.")]
    public string? Note { get; set; }
}

public record SubmitPageViewModel(SubmitForm Form, IReadOnlyList<StoryboardTag> Tags, IReadOnlyList<string> Mediums)
{
    /// <summary>Set when the storyboard is already in the showcase, to link to it.</summary>
    public int? ShowcasedId { get; init; }
}

/// <param name="Showcased">Which of the submissions' beatmapsets are in the showcase now.</param>
/// <param name="Flash">A message from the last action, such as "Thanks!" after submitting.</param>
public record MySubmissionsViewModel(IReadOnlyList<ShowcaseSubmission> Submissions, IReadOnlySet<int> Showcased, string? Flash);
