import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ShortUrlService } from '../../services/short-url.service';
import { ShortUrlInfoResponse } from '../../models/responses/short-url-info.response';

@Component({
  selector: 'app-url-info',
  templateUrl: './url-info.component.html',
  standalone: false,
})
export class UrlInfo implements OnInit {
  info?: ShortUrlInfoResponse;
  isExpanded = false;

  constructor(
    private route: ActivatedRoute,
    private shortUrlService: ShortUrlService
  ) {}

  ngOnInit() {
    const urlId = Number(this.route.snapshot.paramMap.get('id'));
    this.shortUrlService.getShortUrlInfo({ urlId }).subscribe({
      next: (data) => (this.info = data),
      error: (err) => {
        alert('Failed to load info');
        console.error(err);
      },
    });
  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}
