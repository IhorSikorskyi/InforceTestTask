import { Component, OnInit, OnDestroy } from '@angular/core';
import { ShortUrlService } from '../../services/short-url.service';
import { AuthService } from '../../services/auth.service';
import { Subscription, interval } from 'rxjs';
import { ShortUrlTableResponse } from '../../models/responses/short-url-table.response';
import { ShortUrlInfoResponse } from '../../models/responses/short-url-info.response';
import { Router } from '@angular/router';

@Component({
  selector: 'app-short-urls-table',
  templateUrl: './short-urls-table.component.html',
  standalone: false,
})
export class ShortUrlTable implements OnInit, OnDestroy {
  urls: ShortUrlTableResponse[] = [];
  private sub?: Subscription;
  showModal = false;
  longUrl: string = '';
  response?: ShortUrlTableResponse;
  resp?: ShortUrlInfoResponse;

  constructor(
    private shortUrlService: ShortUrlService,
    public authService: AuthService,
    private router: Router // додай це
  ) {}

  ngOnInit() {
    this.loadUrls();
    this.sub = interval(5000).subscribe(() => this.loadUrls());
  }

  ngOnDestroy() {
    this.sub?.unsubscribe();
  }

  loadUrls() {
    this.shortUrlService.getAllShortUrls().subscribe((data) => {
      this.urls = data;
    });
  }

  onInfoClick(url: ShortUrlTableResponse) {
    this.router.navigate(['/info', url.urlId]);
  }

  onRemoveClick(url: ShortUrlTableResponse) {
    const userId = Number(this.authService.getUserIdFromToken());
    const userRole = this.authService.getUserRole();

    if (userRole !== 'Admin' && userId !== url.createdByUserId) {
      alert('You do not have permission to remove this URL.');
      return;
    }

    if (confirm('Are you sure you want to remove this URL?')) {
      this.shortUrlService
        .removeShortUrl({ userId, urlId: url.urlId })
        .subscribe({
          next: () => {
            alert('URL removed successfully.');
            this.loadUrls();
          },
          error: (err) => {
            console.error('Error removing URL:', err);
            alert('Failed to remove the URL.');
          },
        });
    }
  }

  onConfirmClick() {
    if (!this.authService.isLoggedIn()) {
      alert('You need to be logged in to view info.');
      return;
    }
    const userId = this.authService.getUserIdFromToken();
    if (!this.longUrl || !userId) {
      alert('Please enter a valid URL.');
      return;
    }
    this.shortUrlService
      .createShortUrl({ userId, longUrl: this.longUrl })
      .subscribe({
        next: (response) => {
          alert('Short URL created!');
          this.loadUrls();
          this.longUrl = '';
        },
        error: (err) => {
          alert('Failed to create short URL.');
          console.error(err);
        },
      });
  }
}
