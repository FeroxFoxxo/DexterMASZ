import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { ApiEnum } from 'src/app/models/ApiEnum';
import { ApiEnumTypes } from 'src/app/models/ApiEnumTypes';
import { ContentLoading } from 'src/app/models/ContentLoading';
import { DiscordGuild } from 'src/app/models/DiscordGuild';
import { DiscordRole } from 'src/app/models/DiscordRole';
import { GuildConfig } from 'src/app/models/GuildConfig';
import { ApiService } from 'src/app/services/api.service';
import { EnumManagerService } from 'src/app/services/enum-manager.service';

@Component({
  selector: 'app-guild-edit',
  templateUrl: './guild-edit.component.html',
  styleUrls: ['./guild-edit.component.css']
})
export class GuildEditComponent implements OnInit {

  public rolesGroup!: FormGroup;
  public channelsGroup!: FormGroup;
  public configGroup!: FormGroup;

  public allLanguages: ApiEnum[] = [];

  public currentGuild: ContentLoading<DiscordGuild> = { loading: true, content: {} as DiscordGuild }
  public currentGuildConfig: ContentLoading<GuildConfig> = { loading: true, content: {} as GuildConfig }
  constructor(private api: ApiService, private route: ActivatedRoute, private router: Router, private toastr: ToastrService, private _formBuilder: FormBuilder, private translator: TranslateService, private enumManager: EnumManagerService) { }

  ngOnInit(): void {
    this.rolesGroup = this._formBuilder.group({
      adminRoles: ['', Validators.required],
      modRoles: ['', Validators.required]
    });
    this.channelsGroup = this._formBuilder.group({
      staffChannels: [''],
      botChannels: [''],
      staffLogs: '',
      staffAnnouncements: ''
    });
    this.configGroup = this._formBuilder.group({
      strictPermissionCheck: [''],
      executeWhoIsOnJoin: [''],
      publishModeratorInfo: [''],
      preferredLanguage: [''],
    });

    this.route.paramMap.subscribe(params => {
      const guildId = params.get("guildid");
      this.loadLanguages();
      this.loadGuild(guildId);
      this.loadConfig(guildId);
    });
  }

  generateRoleColor(role: DiscordRole): string {
    return '#' + role.color.toString(16);
  }

  loadGuild(id: string|null) {
    this.currentGuild = { loading: true, content: {} as DiscordGuild };
    this.api.getSimpleData(`/discord/guilds/${id}`).subscribe((data: DiscordGuild) => {
      data.roles = data.roles.sort((a, b) => (a.position < b.position) ? 1 : -1);
      data.channels = data.channels.filter(x => x.type === 0).sort((a, b) => (a.position > b.position) ? 1 : -1);
      this.currentGuild = { loading: false, content: data };
    }, error => {
      console.error(error);
      this.currentGuild.loading = false;
      this.toastr.error(this.translator.instant('GuildDialog.FailedToLoadCurrentGuild'));
    });
  }

  loadLanguages() {
    this.enumManager.getEnum(ApiEnumTypes.LANGUAGE).subscribe((data: ApiEnum[]) => {
      this.allLanguages = data;
    });
  }

  loadConfig(id: string|null) {
    this.currentGuildConfig = { loading: true, content: {} as GuildConfig };
    this.api.getSimpleData(`/guilds/${id}`).subscribe((data: GuildConfig) => {
      this.rolesGroup.setValue({
	    modRoles: data.modRoles,
	    adminRoles: data.adminRoles
	  });
      this.channelsGroup.setValue({
	    staffChannels: data.staffChannels,
	    botChannels: data.botChannels,
	    staffLogs: data.staffLogs,
	    staffAnnouncements: data.staffAnnouncements
	  });
      this.configGroup.setValue({
        strictPermissionCheck: data.strictModPermissionCheck,
        executeWhoIsOnJoin: data.executeWhoIsOnJoin,
        publishModeratorInfo: data.publishModeratorInfo,
        preferredLanguage: data.preferredLanguage
      });
      this.currentGuildConfig = { loading: false, content: data };
    }, error => {
      console.error(error);
      this.currentGuildConfig.loading = false;
      this.toastr.error(this.translator.instant('GuildDialog.FailedToLoadCurrentGuild'));
    });
  }

  updateGuild() {
    const data = {
      modRoles: this.rolesGroup.value.modRoles,
      adminRoles: this.rolesGroup.value.adminRoles,
      staffChannels: this.channelsGroup.value.staffChannels != '' ? this.channelsGroup.value.staffChannels : [],
      botChannels: this.channelsGroup.value.botChannels != '' ? this.channelsGroup.value.botChannels : [],
      staffLogs: this.channelsGroup.value?.staffLogs,
      staffAnnouncements: this.channelsGroup.value?.staffAnnouncements,
      strictModPermissionCheck: (this.configGroup.value?.strictPermissionCheck != '' ? this.configGroup.value?.strictPermissionCheck : false) ?? false,
      executeWhoIsOnJoin: (this.configGroup.value?.executeWhoIsOnJoin != '' ? this.configGroup.value?.executeWhoIsOnJoin : false) ?? false,
      publishModeratorInfo: (this.configGroup.value?.publishModeratorInfo != '' ? this.configGroup.value?.publishModeratorInfo : false) ?? false,
      preferredLanguage: this.configGroup.value?.preferredLanguage != '' ? this.configGroup.value?.preferredLanguage : 0
    }

    this.api.putSimpleData(`/guilds/${this.currentGuild?.content?.id}`, data).subscribe(() => {
      this.toastr.success(this.translator.instant('GuildDialog.GuildUpdated'));
      this.router.navigate(['guilds']);
    }, error => {
      console.error(error);
      this.toastr.error(this.translator.instant('GuildDialog.FailedToUpdateGuild'));
    })
  }
}
