import { DiscordChannel } from "./DiscordChannel";
import { DiscordRole } from "./DiscordRole";

export interface DiscordGuild {
  id: string;
  name: string;
  iconUrl: string
  roles: DiscordRole[];
  channels: DiscordChannel[];
}
