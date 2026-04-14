namespace Valtuutus.Playground.Services;

/// <summary>
/// A named preset that pre-populates the playground with a ready-to-use schema and seed data.
/// </summary>
public sealed record PresetSchema(
    string Name,
    string Schema,
    string SeedTuples,
    string SeedAttributes = "");

/// <summary>
/// Built-in playground presets covering common authorization patterns.
/// </summary>
public static class PresetRegistry
{
    // -------------------------------------------------------------------------
    // 1. GitHub
    // -------------------------------------------------------------------------
    public static readonly PresetSchema GitHub = new(
        Name: "GitHub",
        Schema: """
entity user {}

entity organization {
    relation admin @user;
    relation member @user;

    permission manage_org := admin;
}

entity team {
    relation org        @organization;
    relation maintainer @user;
    relation member     @user;

    permission manage_team := maintainer or org.admin;
}

entity repository {
    relation org  @organization;
    relation team @team;

    relation admin      @user;
    relation maintainer @user;
    relation reader     @user;

    permission push   := maintainer or admin or org.admin;
    permission pull   := reader or maintainer or admin or org.admin;
    permission manage := admin or org.admin;
}
""",
        SeedTuples: """
organization:linux#admin@user:torvalds
organization:linux#member@user:alice
organization:linux#member@user:bob
repository:linux#org@organization:linux
repository:linux#admin@user:torvalds
repository:linux#maintainer@user:alice
repository:linux#reader@user:bob
"""
    );

    // -------------------------------------------------------------------------
    // 2. Google Drive
    // -------------------------------------------------------------------------
    public static readonly PresetSchema GoogleDrive = new(
        Name: "Google Drive",
        Schema: """
entity user {}

entity document {
    relation owner  @user;
    relation editor @user;
    relation viewer @user;

    permission read  := viewer or editor or owner;
    permission write := editor or owner;
    permission own   := owner;
}
""",
        SeedTuples: """
document:readme#owner@user:alice
document:readme#editor@user:bob
document:readme#viewer@user:charlie
"""
    );

    // -------------------------------------------------------------------------
    // 3. RBAC (role-based via tuple-to-userset)
    // -------------------------------------------------------------------------
    public static readonly PresetSchema Rbac = new(
        Name: "RBAC",
        Schema: """
entity user {}

entity role {
    relation assignee @user;
}

entity resource {
    relation admin  @role#assignee;
    relation editor @role#assignee;
    relation viewer @role#assignee;

    permission manage := admin;
    permission write  := editor or admin;
    permission read   := viewer or editor or admin;
}
""",
        SeedTuples: """
role:admin_role#assignee@user:alice
role:editor_role#assignee@user:bob
role:viewer_role#assignee@user:charlie
resource:api#admin@role:admin_role#assignee
resource:api#editor@role:editor_role#assignee
resource:api#viewer@role:viewer_role#assignee
"""
    );

    // -------------------------------------------------------------------------
    // 4. Slack
    // -------------------------------------------------------------------------
    public static readonly PresetSchema Slack = new(
        Name: "Slack",
        Schema: """
entity user {}

entity workspace {
    relation admin  @user;
    relation member @user;

    permission manage := admin;
}

entity channel {
    relation workspace @workspace;
    relation owner     @user;
    relation member    @user;

    permission post := member or owner or workspace.admin;
    permission read := member or owner or workspace.admin;
}
""",
        SeedTuples: """
workspace:acme#admin@user:alice
workspace:acme#member@user:bob
workspace:acme#member@user:charlie
channel:general#workspace@workspace:acme
channel:general#owner@user:alice
channel:general#member@user:bob
channel:eng#workspace@workspace:acme
channel:eng#owner@user:alice
channel:eng#member@user:charlie
"""
    );

    // -------------------------------------------------------------------------
    // 5. IoT / Smart Home
    // -------------------------------------------------------------------------
    public static readonly PresetSchema IoTSmartHome = new(
        Name: "IoT / Smart Home",
        Schema: """
entity user {}

entity home {
    relation owner  @user;
    relation member @user;

    permission manage := owner;
    permission access := member or owner;
}

entity device {
    relation home @home;

    permission control := home.owner;
    permission view    := home.owner or home.member;
}
""",
        SeedTuples: """
home:smiths#owner@user:alice
home:smiths#member@user:bob
device:thermostat#home@home:smiths
device:camera#home@home:smiths
"""
    );

    // -------------------------------------------------------------------------
    // 6. Social / Twitter
    // -------------------------------------------------------------------------
    public static readonly PresetSchema SocialTwitter = new(
        Name: "Social / Twitter",
        Schema: """
entity user {
    relation follower @user;

    permission follow := follower;
}

entity post {
    relation author @user;
    relation viewer @user;

    attribute is_public bool;

    permission view := author or viewer or check_public(is_public);
    permission edit := author;
}

fn check_public(is_public bool) => is_public == true;
""",
        SeedTuples: """
user:alice#follower@user:bob
post:post1#author@user:alice
post:post2#author@user:alice
""",
        SeedAttributes: """
post:post1$is_public=true
post:post2$is_public=false
"""
    );

    // -------------------------------------------------------------------------
    // All presets — declared last so all static field references are resolved
    // -------------------------------------------------------------------------
    public static IReadOnlyList<PresetSchema> All { get; } = new List<PresetSchema>
    {
        GitHub,
        GoogleDrive,
        Rbac,
        Slack,
        IoTSmartHome,
        SocialTwitter,
    };
}
