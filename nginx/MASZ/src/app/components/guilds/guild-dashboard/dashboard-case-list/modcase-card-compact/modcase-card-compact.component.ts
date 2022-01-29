import { Component, Input, OnInit } from '@angular/core';
import { ApiEnumTypes } from 'src/app/models/ApiEnumTypes';
import { ApiEnum } from 'src/app/models/ApiEnum';
import { ContentLoading } from 'src/app/models/ContentLoading';
import { convertModCaseToPunishmentString } from 'src/app/models/ModCase';
import { IModCaseTableEntry } from 'src/app/models/IModCaseTableEntry';
import { PunishmentType } from 'src/app/models/PunishmentType';
import { EnumManagerService } from 'src/app/services/enum-manager.service';
import * as moment from 'moment';

@Component({
  selector: 'app-modcase-card-compact',
  templateUrl: './modcase-card-compact.component.html',
  styleUrls: ['./modcase-card-compact.component.css']
})
export class ModCaseCardCompactComponent implements OnInit {

  @Input() entry!: IModCaseTableEntry;
  @Input() showExpiring: boolean = true;
  @Input() showCreated: boolean = false;

  public punishmentTooltip: string = "";
  public punishment: string = "Unknown";
  public punishments: ContentLoading<ApiEnum[]> = { loading: true, content: [] };

  constructor(private enumManager: EnumManagerService) { }

  ngOnInit(): void {
    if (this.entry.modCase.punishmentType !== PunishmentType.Warn && ! this.entry.modCase.punishmentActive) {
      if (this.entry.modCase.punishedUntil === null) {
        this.punishmentTooltip = "ModCase deactivated.";
      } else if (moment(this.entry.modCase.punishedUntil).utc(true).isAfter(moment())) {
        this.punishmentTooltip = "ModCase deactivated.";
      } else {
        this.punishmentTooltip = "ModCase expired.";
      }
    }
    this.enumManager.getEnum(ApiEnumTypes.PUNISHMENT).subscribe((data) => {
      this.punishments.loading = false;
      this.punishments.content = data;
      this.punishment = convertModCaseToPunishmentString(this.entry?.modCase, this.punishments.content);
    }, () => {
      this.punishments.loading = false;
    });
  }

}
