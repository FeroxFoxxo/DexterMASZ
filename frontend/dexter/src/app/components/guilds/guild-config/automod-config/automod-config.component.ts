import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { AutoModConfig } from 'src/app/models/AutoModConfig';
import { AutoModRuleDefinition } from 'src/app/models/AutoModRuleDefinitions';
import { DiscordChannel } from 'src/app/models/DiscordChannel';
import { DiscordGuild } from 'src/app/models/DiscordGuild';
import { ApiService } from 'src/app/services/api.service';

@Component({
  selector: 'app-automod-config',
  templateUrl: './automod-config.component.html',
  styleUrls: ['./automod-config.component.css']
})
export class AutoModConfigComponent implements OnInit {
  types: AutoModRuleDefinition[] = [
    {
      type: 0,
      key: 'Invites',
      showLimitField: false,
      showTimeLimitField: false,
      showCustomField: true,
      requireCustomField: false,
    },
    {
      type: 1,
      key: 'Emotes',
      showLimitField: true,
      showTimeLimitField: false
    },
    {
      type: 2,
      key: 'Mentions',
      showLimitField: true,
      showTimeLimitField: false
    },
    {
      type: 3,
      key: 'Attachments',
      showLimitField: true,
      showTimeLimitField: false
    },
    {
      type: 4,
      key: 'Embeds',
      showLimitField: true,
      showTimeLimitField: false
    },
    {
      type: 5,
      key: 'Automods',
      showLimitField: true,
      showTimeLimitField: true
    },
    {
      type: 6,
      key: 'CustomFilter',
      showLimitField: true,
      showTimeLimitField: false,
      showCustomField: true,
      tooltip: true,
      link: 'https://gist.github.com/zaanposni/4f3aa7b29d54005d34eb78f6acfe93eb',
      requireCustomField: true
    },
    {
      type: 7,
      key: 'Spam',
      showLimitField: true,
      showTimeLimitField: true,
      timeLimitFieldMessage: true
    },
    {
      type: 8,
      key: 'DuplicatedChars',
      showLimitField: true,
      showTimeLimitField: false
    },
    {
      type: 9,
      key: 'Link',
      showLimitField: true,
      showTimeLimitField: false,
      showCustomField: true,
      requireCustomField: false,
      link: 'https://gist.github.com/zaanposni/5808c07c26ba04f81a9ef31c6dfa3a7e'
    }
  ];

  public guildId!: string;
  public guildInfo!: DiscordGuild;
  public guildChannels!: DiscordChannel[];
  public initialConfigs!: Promise<AutoModConfig[]>;

  constructor(private api: ApiService, private toastr: ToastrService, private route: ActivatedRoute, private translator: TranslateService) { }

  ngOnInit(): void {
    this.guildId = this.route.snapshot.paramMap.get('guildid') as string;
    this.reload();
  }

  reload() {
    this.api.getSimpleData(`/discord/guilds/${this.guildId}`).subscribe((data: DiscordGuild) => {
      data.roles = data.roles.sort((a, b) => (a.position < b.position) ? 1 : -1);
      this.guildInfo = data;
    }, () => {
      this.toastr.error(this.translator.instant('AutoModConfig.FailedToLoadGuild'));
    });
    this.api.getSimpleData(`/discord/guilds/${this.guildId}/textableChannels`).subscribe((data: DiscordChannel[]) => {
      this.guildChannels = data.sort((a, b) => (a.position > b.position) ? 1 : -1);
    }, () => {
      this.toastr.error(this.translator.instant('AutoModConfig.FailedToLoadChannels'));
    });
    this.initialConfigs = this.api.getSimpleData(`/guilds/${this.guildId}/automodconfig`).toPromise();
  }
}
