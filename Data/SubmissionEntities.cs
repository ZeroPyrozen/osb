#nullable enable

namespace osb.Data;

/// <summary>Where a showcase submission is in review. Approved, Declined and Withdrawn are final.</summary>
public enum SubmissionStatus
{
    /// <summary>Waiting for a reviewer ("Waiting for review").</summary>
    Pending = 0,

    /// <summary>Added to the showcase ("Showcased").</summary>
    Approved = 1,

    /// <summary>Declined, with a message for the member.</summary>
    Declined = 2,

    /// <summary>Taken back by the member who submitted it.</summary>
    Withdrawn = 3,
}

/// <summary>
/// A storyboard a member suggested for the showcase. The title, artist, mapper and osu! submission
/// date are copied from osu! when it's submitted, so reviewers don't need osu! to read it. Approving
/// it adds a <see cref="Beatmapset"/>; reviewers can change anything first.
/// </summary>
public class ShowcaseSubmission
{
    public int Id { get; set; }

    /// <summary>The osu! beatmapset ID.</summary>
    public int BeatmapsetId { get; set; }

    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";

    /// <summary>The mapper (the beatmapset's host), as osu! had them.</summary>
    public int HostId { get; set; }

    public string HostUsername { get; set; } = "";

    /// <summary>When the beatmapset was submitted to osu!.</summary>
    public DateOnly BeatmapSubmittedOn { get; set; }

    /// <summary>Whether osu! lists a storyboard for the beatmapset. Reviewers see a warning when it doesn't.</summary>
    public bool OsuListsStoryboard { get; set; }

    /// <summary>The tool or language the storyboard was made with.</summary>
    public string Medium { get; set; } = "";

    /// <summary>A YouTube embed URL, if the member gave a video.</summary>
    public string? VideoUrl { get; set; }

    /// <summary>The member's note to the reviewers.</summary>
    public string? Note { get; set; }

    /// <summary>The storyboarders, in credit order.</summary>
    public List<SubmissionCredit> Storyboarders { get; set; } = new();

    /// <summary>Tags the member suggests. Reviewers choose the final tags, which set the OSB level.</summary>
    public List<StoryboardTag> SuggestedTags { get; set; } = new();

    public int SubmitterId { get; set; }
    public string SubmitterUsername { get; set; } = "";

    /// <summary>When the member submitted it (UTC).</summary>
    public DateTime SubmittedAt { get; set; }

    public SubmissionStatus Status { get; set; }

    public int? ReviewerId { get; set; }
    public string? ReviewerUsername { get; set; }

    /// <summary>When it was approved, declined or withdrawn (UTC).</summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>The reviewer's message to the member. Declining needs one.</summary>
    public string? ReviewNote { get; set; }

    /// <summary>
    /// Whether the member has seen how it was reviewed. Until they have, their account menu shows a
    /// dot next to My submissions.
    /// </summary>
    public bool OutcomeSeen { get; set; }

    /// <summary>Storyboarders in credit order.</summary>
    public IEnumerable<SubmissionCredit> Credits => Storyboarders.OrderBy(c => c.Position);
}

/// <summary>A storyboarder credited on a submission, as osu! had them. <see cref="Position"/> keeps the credit order.</summary>
public class SubmissionCredit
{
    public int SubmissionId { get; set; }
    public ShowcaseSubmission Submission { get; set; } = null!;

    /// <summary>The storyboarder's osu! user ID. They may not be known to the site yet.</summary>
    public int OsuUserId { get; set; }

    public string Username { get; set; } = "";
    public int Position { get; set; }
}

/// <summary>
/// A storyboard a reviewer removed from the showcase. The start-up import skips it, so showcase.json
/// can't bring it back; approving a new submission for it can.
/// </summary>
public class ShowcaseRemoval
{
    /// <summary>The osu! beatmapset ID.</summary>
    public int BeatmapsetId { get; set; }

    /// <summary>When it was removed (UTC).</summary>
    public DateTime RemovedAt { get; set; }

    public int RemovedById { get; set; }
    public string RemovedByUsername { get; set; } = "";
    public string Reason { get; set; } = "";
}
