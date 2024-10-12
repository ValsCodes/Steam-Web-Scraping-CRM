import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { SellListingComponent } from '../sell-listing/sell-listing.component';

@Component({
  selector: 'steam-manual-mode',
  standalone: true,
  imports: [SellListingComponent, FormsModule],
  templateUrl: './manual-mode.component.html',
  styleUrl: './manual-mode.component.scss',
})
export class ManualModeComponent {
  private readonly HATS_URL: string =
    'https://steamcommunity.com/market/search?q=&category_440_Collection%5B%5D=any&category_440_Type%5B%5D=tag_misc&category_440_Quality%5B%5D=tag_Unique&category_440_Quality%5B%5D=tag_strange&appid=440#';
  private readonly WEAPONS_URL: string =
    'https://steamcommunity.com/market/listings/440/Strange%20Specialized%20Killstreak%20';

  private readonly WEAPON_NAMES: string[] = [
    'Degreaser',
    'Backburner',
    'Phlogistinator',
    'Flame%20Thrower',
    'Scattergun',
    'Force-A-Nature',
    'Guilotine',
    'Rocket%20Launcher',
    'Direct%20Hit',
    'Black%20Box',
    'Minigun',
    'Rescue%20Ranger',
    'Crusader%27s%20Crossbow',
    'Sniper%20Rifle',
    'L%27Etranger',
    'Shotgun',
    'Spy-cicle',
    'Tomislav',
    'Medi%20Gun',
    'Axtinguisher',
    'Kukri',
    'Powerjack',
    'Fists%20of%20Steel',
    'Bushwacka',
    'Your%20Eternal%20Reward',
    'Gloves%20of%20Running%20Urgently',
    'Detonator',
    'SMG',
    'Holiday%20Punch',
    'Jag',
    'Conniver%27s%20Kunai',
    'Escape%20Plan',
    'Scorch%20Shot',
    'Wrench',
    'Disciplinary%20Action',
    'Flare%20Gun',
    'Eyelander',
    'Hitman%27s%20Heatmaker',
    'Quick-Fix',
    'Knife',
    'Market%20Gardener',
    'Backburner',
    'Bottle',
    'Machina',
    'Revolver',
    'Ambassador',
    'Pistol',
    'Wrangler',
    'Stickybomb%20Launcher',
    'Frontier%20Justice',
    'Kritzkrieg',
    'Huntsman',
    'Grenade%20Launcher',
    'Cleaner%27s%20Carbine',
    'Quickiebomb%20Launcher',
    'Ubersaw',
  ];

  private currentUrl: string = this.HATS_URL;
  public isHatsSearch: boolean = true;
  public batchSize: number = 7;
  public currentPage: number = 76;
  public statusLabel: string = '';

  startButtonClicked(): void {
    let toPage = this.currentPage + this.batchSize;
    let blocked = false;

    for (; this.currentPage < toPage; this.currentPage++) {
      let url = this.isHatsSearch
        ? `${this.currentUrl}p${this.currentPage}_price_asc`
        : this.WEAPON_NAMES.length > this.currentPage
        ? `${this.currentUrl}${this.WEAPON_NAMES[this.currentPage - 1]}`
        : null;

      if (url == null) {
        this.statusLabel = 'No More Weapons!'
        return;
      }

      let newWindow = window.open(url, '_blank');

      if (
        !newWindow ||
        newWindow.closed ||
        typeof newWindow.closed === 'undefined'
      ) {
        blocked = true;
      }
    }


    this.statusLabel = blocked ? 'Bad Batch!' : 'Successful Batch!';

    if (blocked) {
      alert(
        'Pop-ups were blocked. Please enable pop-ups for this website to open all pages.'
      );
    }
  }

  hatsButtonClicked() {
    if (!this.isHatsSearch) {
      this.currentUrl = this.HATS_URL;
      this.isHatsSearch = true;

      this.resetButtonClicked();
    }
  }

  weaponsButtonClicked() {
    if (this.isHatsSearch) {
      this.currentUrl = this.WEAPONS_URL;
      this.isHatsSearch = false;

      this.resetButtonClicked();
    }
  }

  resetButtonClicked(): void {
    if (this.isHatsSearch) {
      this.currentPage = 76;
      this.batchSize = 7;
    } else {
      this.currentPage = 1;
      this.batchSize = 3;
    }

    this.statusLabel = '';
  }
}
