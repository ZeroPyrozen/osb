#nullable enable

using System.ComponentModel.DataAnnotations;
using osb.Data;

namespace osb.ViewModels;

/// <summary>
/// A storyboard's showcase entry as a reviewer fills it in, when approving a submission or editing a
/// showcased storyboard.
/// </summary>
public class EntryForm
{
    [Required(ErrorMessage = "Give the song's title.")]
    [StringLength(256, ErrorMessage = "Keep the title to 256 characters.")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Give the artist.")]
    [StringLength(256, ErrorMessage = "Keep the artist to 256 characters.")]
    public string Artist { get; set; } = "";

    /// <summary>The mapper (the beatmapset's host): an osu! username, ID or profile link.</summary>
    [Required(ErrorMessage = "Give the mapper's osu! username.")]
    [StringLength(300, ErrorMessage = "That's too long for a username.")]
    public string Mapper { get; set; } = "";

    [Required(ErrorMessage = "List at least one storyboarder.")]
    [StringLength(1000, ErrorMessage = "That's too long for a list of storyboarders.")]
    public string Storyboarders { get; set; } = "";

    [Required(ErrorMessage = "Say which tool it was made with.")]
    [StringLength(32, ErrorMessage = "Keep the tool's name to 32 characters.")]
    public string Medium { get; set; } = "";

    [StringLength(300, ErrorMessage = "That's too long for a video link.")]
    public string? Video { get; set; }

    public List<string> Tags { get; set; } = new();

    /// <summary>When the beatmapset was submitted to osu!.</summary>
    [Required(ErrorMessage = "Give the date it was submitted to osu!.")]
    public DateOnly? SubmittedOn { get; set; }

    [Required(ErrorMessage = "Give the date it joins the showcase.")]
    public DateOnly? ShowcasedOn { get; set; }
}

/// <summary>The review form: the showcase entry, plus a message for the member who submitted it.</summary>
public class ReviewForm : EntryForm
{
    [StringLength(1000, ErrorMessage = "Keep the message to 1,000 characters.")]
    public string? Message { get; set; }
}

/// <summary>The entry fields' form plus the choices they offer.</summary>
/// <param name="TagsHint">Says which tags start ticked.</param>
public record EntryFieldsViewModel(EntryForm Form, IReadOnlyList<StoryboardTag> Tags, IReadOnlyList<string> Mediums, string TagsHint);

public record ReviewQueueViewModel(IReadOnlyList<ShowcaseSubmission> Waiting, IReadOnlyList<ShowcaseSubmission> Recent, string? Flash);

/// <param name="EarlierAttempts">Other submissions of the same beatmapset, newest first.</param>
/// <param name="NewPeople">People the submission names who aren't on the site yet.</param>
/// <param name="Removal">Set when a reviewer took the storyboard out of the showcase before.</param>
public record ReviewPageViewModel(
    ShowcaseSubmission Submission,
    ReviewForm Form,
    IReadOnlyList<StoryboardTag> Tags,
    IReadOnlyList<string> Mediums,
    IReadOnlyList<ShowcaseSubmission> EarlierAttempts,
    IReadOnlyList<string> NewPeople,
    ShowcaseRemoval? Removal)
{
    public EntryFieldsViewModel Fields => new(Form, Tags, Mediums, "The member's suggestions are ticked.");
}

/// <summary>The edit page of a showcased storyboard, which can also remove it.</summary>
/// <param name="Reason">The reason typed into the removal form, when removing needs another try.</param>
public record EditPageViewModel(Beatmapset Set, EntryForm Form, IReadOnlyList<StoryboardTag> Tags, IReadOnlyList<string> Mediums, string? Reason = null)
{
    public EntryFieldsViewModel Fields => new(Form, Tags, Mediums, "Its current tags are ticked.");
}
