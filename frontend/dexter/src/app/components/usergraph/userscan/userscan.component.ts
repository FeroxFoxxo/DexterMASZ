import { HttpParams } from '@angular/common/http';
import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { ApiEnum } from 'src/app/models/ApiEnum';
import { ApiEnumTypes } from 'src/app/models/ApiEnumTypes';
import { AutoModEvent } from 'src/app/models/AutoModEvent';
import { ContentLoading } from 'src/app/models/ContentLoading';
import { DiscordGuild } from 'src/app/models/DiscordGuild';
import { DiscordUser } from 'src/app/models/DiscordUser';
import { InviteNetwork } from 'src/app/models/InviteNetwork';
import { convertModCaseToPunishmentString, ModCase } from 'src/app/models/ModCase';
import { UserInvite } from 'src/app/models/UserInvite';
import { UserNetwork } from 'src/app/models/UserNetwork';
import { UserNote } from 'src/app/models/UserNote';
import { ApiService } from 'src/app/services/api.service';
import { EnumManagerService } from 'src/app/services/enum-manager.service';
import { TimezoneService } from 'src/app/services/timezone.service';
import { Edge, Network, Node } from 'vis';

@Component({
  selector: 'app-userscan',
  templateUrl: './userscan.component.html',
  styleUrls: ['./userscan.component.css']
})
export class UserScanComponent implements AfterViewInit, OnInit {

  @ViewChild('network') el!: ElementRef;
  timeout: any = null;
  loading: boolean = false;
  public search!: string;
  private networkInstance!: Network;
  public showUsage: boolean = true;
  private options = {
    height: '100%',
    width: '100%',
    nodes: {
      shape: 'dot',
      size: 30,
      borderWidth: 3,
      shadow: true,
      font: {
        color: 'white'
      }
    },
    edges: {
      color: { inherit: 'to' },
      smooth: {
        enabled: true,
        type: 'continuous',
        roundness: 0.1
      }
    },
    interaction: {
      hover: true
    }
  };
  private data: {'nodes': Node[], 'edges': Edge[]} = { 'nodes': [], 'edges': [] };
  private punishments: ContentLoading<ApiEnum[]> = { loading: true, content: [] };
  private automodtypes: ContentLoading<ApiEnum[]> = { loading: true, content: [] };

  constructor(private api: ApiService, private toastr: ToastrService, private route: ActivatedRoute, private enumManager: EnumManagerService, private translator: TranslateService, private timezoneService: TimezoneService) { }

  ngOnInit(): void {
    window.scrollTo(0, 0);
    this.reloadPunishmentEnum();
    this.reloadAutoModTypeEnum();
  }

  reloadPunishmentEnum() {
    this.punishments.loading = true;
    this.enumManager.getEnum(ApiEnumTypes.PUNISHMENT).subscribe((data: ApiEnum[]) => {
      this.punishments.loading = false;
      this.punishments.content = data;
    }, () => {
      this.punishments.loading = false;
    });
  }

  reloadAutoModTypeEnum() {
    this.automodtypes.loading = true;
    this.enumManager.getEnum(ApiEnumTypes.AUTOMODTYPE).subscribe((data: ApiEnum[]) => {
      this.automodtypes.loading = false;
      this.automodtypes.content = data;
    }, () => {
      this.automodtypes.loading = false;
    });
  }

  onSearch(event: any) {
    clearTimeout(this.timeout);
    var $this = this;
    this.timeout = setTimeout(function () {
      if (event.keyCode != 13) {
        $this.executeSearch();
      }
    }, 100);
  }

  executeSearch() {
    if (this.search?.trim()) {
      this.showUsage = false;
      this.loading = true;
      this.reset();

      let searchString = this.search?.trim();

      let userIdRegex = new RegExp('[0-9]{16,20}', 'i');
      if (userIdRegex.test(searchString)) {
        this.loadDataForUserId(searchString).subscribe((data: UserNetwork) => {
          this.calculateNewUserNetwork(data, this.search?.trim());
          this.loading = false;
        }, error => {
          console.error(error);
          this.reset();
          this.loading = false;
          this.toastr.error(this.translator.instant('Scanning.FailedToLoad.Scan'));
        });

      } else {
        searchString = searchString.substring(searchString.lastIndexOf('/') + 1)
        this.loadDataForInvite(`https://discord.gg/${searchString}`).subscribe((data: InviteNetwork) => {
          this.calculateNewInviteNetwork(data, this.search?.trim());
          this.loading = false;
        }, error => {
          this.reset();
          this.loading = false;
          if (error?.error?.status === 404 || error?.status === 404) {
            this.toastr.error(this.translator.instant('Scanning.FailedToLoad.InvalidInvite'));
          } else {
            console.error(error);
            this.toastr.error(this.translator.instant('Scanning.FailedToLoad.Invite'));
          }
        });
      }

    } else {
      this.reset();
    }
  }

  loadDataForUserId(userId: string): Observable<UserNetwork> {
    let params = new HttpParams().set('userId', userId);
    return this.api.getSimpleData(`/network/user`, true, params);
  }

  loadDataForInvite(inviteUrl: string): Observable<InviteNetwork> {
    let params = new HttpParams().set('inviteUrl', inviteUrl);
    return this.api.getSimpleData(`/network/invite`, true, params);
  }

  ngAfterViewInit() {
     const container = this.el.nativeElement;
     this.networkInstance = new Network(container, this.data, this.options);
     this.networkInstance.on("doubleClick", this.onDoubleClick.bind(this));
     if ('search' in this.route.snapshot.queryParams) {
      this.search = this.route.snapshot.queryParams['search'];
      this.executeSearch();
    }
  }

  onDoubleClick(params: any) {
    if (params?.nodes?.length === 1) {
      let node = this.data.nodes.find(x => x.id === params?.nodes.First()) as any;
      if (node?.searchFor) {
        this.search = node?.searchFor;
        this.executeSearch();
        return;
      }
      if (node?.redirectTo) {
        window.open(node.redirectTo, '_blank');
      }
    }
  }

  reset() {
    this.data.nodes = [];
    this.data.edges = [];
    this.redraw();
  }

  calculateNewInviteNetwork(network: InviteNetwork, searchString: string) {
    let guild = network?.guild;
    if (!guild) return;

    let baseNode = this.addNewNode(this.newGuildNode, [guild, guild.id, 40, `${searchString}/`]) as Node;

    for (let invite of network.invites) {
      if (invite.userInvite.guildId !== guild.id) continue;
      let inviteNode = this.addNewNode(this.newInviteNode, [invite.userInvite]) as Node;
      this.addNewEdge(baseNode, inviteNode, '', false, 'no');
      if (invite?.userInvite?.inviteIssuerId !== "0")
      {
        let inviterUserNode = this.addNewNode(this.newUserNode, [invite?.invitedBy, invite?.userInvite?.inviteIssuerId, 50]) as Node;
        this.addNewEdge(inviteNode, inviterUserNode, `${this.translator.instant('Scanning.CreatedAt')}: ${this.timezoneService.convertNearlyAnyDateToLocaleString(invite.userInvite.inviteCreatedAt)}`, false, 'from');
      }
      let invitedUserNode = this.addNewNode(this.newUserNode, [invite?.invitedUser, invite?.userInvite?.joinedUserId]) as Node;
      this.addNewEdge(inviteNode, invitedUserNode, `${this.translator.instant('Scanning.JoinedAt')}: ${this.timezoneService.convertNearlyAnyDateToLocaleString(invite.userInvite.joinedAt)}`, true, 'to');
    }

    this.redraw();
  }

  calculateNewUserNetwork(network: UserNetwork, userId: string) {
    let baseNode = this.addNewNode(this.newUserNode, [network?.user, userId, 50, 'basics']) as Node;
    for (let guild of network.guilds) {
      let guildNode = this.addNewNode(this.newGuildNode, [guild, guild.id, 40, `${userId}/`]) as Node;
      this.addNewEdge(baseNode, guildNode);
      for (let invite of network.invitedBy) {
        if (invite.userInvite.guildId !== guild.id) continue;
        let inviteNode = this.addNewNode(this.newInviteNode, [invite.userInvite]) as Node;
        this.addNewEdge(guildNode, inviteNode, `${this.translator.instant('Scanning.JoinedAt')}: ${this.timezoneService.convertNearlyAnyDateToLocaleString(invite.userInvite.joinedAt)}`, true, 'from');
        if (invite?.userInvite?.inviteIssuerId !== "0")
        {
          let invitedUserNode = this.addNewNode(this.newUserNode, [invite?.invitedBy, invite?.userInvite?.inviteIssuerId]) as Node;
          this.addNewEdge(inviteNode, invitedUserNode, `${this.translator.instant('Scanning.CreatedAt')}: ${this.timezoneService.convertNearlyAnyDateToLocaleString(invite.userInvite.inviteCreatedAt)}`, false, 'from');
        }
      }
      for (let invite of network.invited) {
        if (invite.userInvite.guildId !== guild.id) continue;
        let inviteNode = this.addNewNode(this.newInviteNode, [invite.userInvite]) as Node;
        this.addNewEdge(guildNode, inviteNode, `${this.translator.instant('Scanning.CreatedAt')}: ${this.timezoneService.convertNearlyAnyDateToLocaleString(invite.userInvite.inviteCreatedAt)}`, false, 'to');
        if ( invite.userInvite.joinedUserId !== invite.userInvite.inviteIssuerId ) {
          let invitedUserNode = this.addNewNode(this.newUserNode, [invite?.invitedUser, invite?.userInvite?.joinedUserId]) as Node;
          this.addNewEdge(inviteNode, invitedUserNode, `${this.translator.instant('Scanning.JoinedAt')}: ${this.timezoneService.convertNearlyAnyDateToLocaleString(invite.userInvite.joinedAt)}`, true, 'to');
        }
      }
      for (let modCase of network.modCases) {
        if (modCase.guildId !== guild.id) continue;
        let caseBaseNode = this.addNewNode(this.newBasicCasesNode, [userId, guild.id]) as Node;
        this.addNewEdge(guildNode, caseBaseNode);
        let caseNode = this.addNewNode(this.newCaseNode, [modCase]) as Node;
        this.addNewEdge(caseBaseNode, caseNode, `${this.translator.instant('Scanning.CreatedAt')}: ${this.timezoneService.convertNearlyAnyDateToLocaleString(modCase.createdAt)}`);
      }
      for (let modEvent of network.modEvents) {
        if (modEvent.guildId !== guild.id) continue;
        let eventBaseNode = this.addNewNode(this.newBasicAutomodsNode, [userId, guild.id]) as Node;
        this.addNewEdge(guildNode, eventBaseNode);
        let eventNode = this.addNewNode(this.newEventNode, [modEvent, 5]) as Node;
        this.addNewEdge(eventBaseNode, eventNode, `${this.translator.instant('Scanning.OccuredAt')}: ${this.timezoneService.convertNearlyAnyDateToLocaleString(modEvent.createdAt)}`);
      }
      for (let note of network.userNotes) {
        if (note.guildId !== guild.id) continue;
        let noteNode = this.addNewNode(this.newNoteNode, [note]) as Node;
        this.addNewEdge(guildNode, noteNode, '', false, 'no', 250);
      }
      for (let userMap of network.userMaps) {
        if (userMap.userMap.guildId !== guild.id) continue;
        let mapBaseNode = this.addNewNode(this.newBasicMapNode, [userId, userMap.userMap.guildId]) as Node;
        this.addNewEdge(guildNode, mapBaseNode);
        if (userMap.userMap.userA !== userId) {
          let userNode = this.addNewNode(this.newUserNode, [userMap.userA, userMap.userMap.userA]) as Node;
          this.addNewEdge(mapBaseNode, userNode, userMap.userMap.reason, true, 'to');
        }
        else if (userMap.userMap.userB !== userId) {
          let userNode = this.addNewNode(this.newUserNode, [userMap.userB, userMap.userMap.userB]) as Node;
          this.addNewEdge(mapBaseNode, userNode, userMap.userMap.reason, true, 'to');
        }
      }
    }
    this.redraw();
  }

  redraw() {
    this.networkInstance.destroy
    this.networkInstance.setData(this.data);
    this.networkInstance.redraw();
  }

  addNewNode(func: CallableFunction, params: any[]): Node|Node[] {
    let newNode = func.bind(this)(...params);
    if (Array.isArray(newNode)) {
      newNode.forEach(element => {
        if (this.data.nodes.filter(x => x.id === element?.id)?.length === 0) {
          this.data.nodes.push(element);
        }
      });
    } else {
      if (this.data.nodes.filter(x => x.id === newNode?.id)?.length === 0) {
        this.data.nodes.push(newNode);
      }

    }
    return newNode;
  }

  addNewEdge(from: Node, to: Node, title: string = '', addWithRoundness: boolean = false, arrow: 'to'|'from'|'no' = 'no', length: number = 160): Edge {
    let newEdge = {id: `${from.id}/edge/${to.id}`, from: from.id, to: to.id, title: title.trim() === '' ? undefined : title, length: length} as any;
    if (arrow === 'to') {
      newEdge['arrows'] = { middle: { scaleFactor: 0.5 }, to: true };
    }
    if (arrow === 'from') {
      newEdge['arrows'] = { middle: { scaleFactor: 0.5 }, from: true };
    }
    let existingEdges = this.data.edges.filter(x => x.id?.toString().startsWith(newEdge?.id));
    if (existingEdges.length === 0) {
      this.data.edges.push(newEdge);
    } else if (addWithRoundness) {
      let roundness = 0;
      for (let edge of existingEdges) {
        let s = edge?.smooth as any;
        if (s && 'roundness' in s) {
          if (s['roundness'] > roundness) roundness = s['roundness'];
        }
      }
      roundness += 0.2;
      newEdge['id'] += `/${roundness}`;
      newEdge['smooth'] = { type: 'curvedCW', roundness: roundness, enabled: true };
      this.data.edges.push(newEdge);
    }
    return newEdge;
  }

  newUserNode(user: DiscordUser, backupUserId: string = "0", size: number = 30, group: string = 'otherusers'): Node {
    return {
      id: (user?.id ?? backupUserId).toString(),
      shape: 'circularImage',
      group: group,
      image: user?.imageUrl ?? '/assets/img/default_profile.png',
      label: user != null ? `${user.username}` : backupUserId.toString(),
      title: (user?.id ?? backupUserId).toString(),
      size: size,
      searchFor: (user?.id ?? backupUserId).toString()
    } as Node
  }

  newGuildNode(guild: DiscordGuild, guildId: string, size: number = 30, idPrefix: string = ''): Node {
    return {
      id: idPrefix + (guild?.id ?? guildId).toString(),
      shape: 'circularImage',
      group: 'basics',
      image: guild?.iconUrl ?? '/assets/img/default_profile.png',
      label: guild?.name ?? guildId,
      title: (guild?.id ?? guildId).toString(),
      size: size
    }
  }

  newNoteNode(userNote: UserNote): Node {
    return {
      id: `${userNote.guildId}/usernote/${userNote.id}`,
      title: userNote.description,
      label: userNote.description,
      shape: 'box'
    }
  }

  newCaseNode(modCase: ModCase, size: number = 20): Node {
    let punishmentString = convertModCaseToPunishmentString(modCase, this.punishments.content);
    if (modCase.punishedUntil != null) {
      punishmentString += `, Until: ${this.timezoneService.convertNearlyAnyDateToLocaleString(modCase.punishedUntil)}`;
    }
    return {
      id: `${modCase.guildId}/case/${modCase.caseId}`,
      label: `${this.translator.instant('Case')} #${modCase.caseId}\n${modCase.title.substr(0, 50)}`,
      title: `${this.translator.instant('Punishment')}: ${punishmentString} ${modCase.description.substr(0, 200)}`,
      group: `${modCase.guildId}/cases`,
      size: size,
      redirectTo: `/guilds/${modCase.guildId}/cases/${modCase.caseId}`
    } as Node
  }

  newEventNode(modEvent: AutoModEvent, size: number = 20): Node {
    return {
      id: `${modEvent.guildId}/automod/${modEvent.id}`,
      group: `${modEvent.guildId}/automods`,
      title: `${this.automodtypes.content?.find(x => x.key === modEvent.autoModType)?.value ?? 'Unknown'} ${modEvent.messageContent}`,
      size: size
    }
  }

  newInviteNode(invite: UserInvite): Node {
    return {
      id: `${invite.guildId}/${invite.usedInvite}/${this.timezoneService.convertNearlyAnyDateToLocaleString(invite.inviteCreatedAt)}`,
      label: invite.usedInvite.substr(invite.usedInvite.lastIndexOf("/") + 1),
      group: `${invite.guildId}/invites`,
      shape: 'diamond',
      title: `${this.translator.instant('Invite')}: ${invite?.usedInvite}`,
      searchFor: invite?.usedInvite,
      size: 15
    } as Node
  }

  newBasicCasesNode(userId: string, guildId: string): Node {
    return {
      id: `${userId}/${guildId}/cases`,
      label: this.translator.instant('Cases'),
      group: `basics/sub`,
      shape: 'triangle',
      size: 15
    }
  }

  newBasicAutomodsNode(userId: string, guildId: string): Node {
    return {
      id: `${userId}/${guildId}/automods`,
      label: this.translator.instant('Automods'),
      group: `basics/sub`,
      shape: 'triangle',
      size: 15
    }
  }

  newBasicMapNode(userId: string, guildId: string): Node {
    return {
      id: `${userId}/${guildId}/userMaps`,
      label: this.translator.instant('UserMaps'),
      group: `basics/sub`,
      shape: 'triangle',
      size: 15
    }
  }
}
