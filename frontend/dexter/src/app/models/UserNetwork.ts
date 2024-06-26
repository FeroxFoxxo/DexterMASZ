import { AutoModEvent } from "./AutoModEvent";
import { DiscordGuild } from "./DiscordGuild";
import { DiscordUser } from "./DiscordUser";
import { ModCase } from "./ModCase";
import { UserInviteExpanded } from "./UserInviteExpanded";
import { UserMapExpanded } from "./UserMapExpanded";
import { UserNote } from "./UserNote";

export interface UserNetwork {
    guilds: DiscordGuild[];
    user : DiscordUser;
    invited: UserInviteExpanded[];
    invitedBy: UserInviteExpanded[];
    modCases: ModCase[];
    modEvents: AutoModEvent[];
    userMaps: UserMapExpanded[];
    userNotes: UserNote[];
}
