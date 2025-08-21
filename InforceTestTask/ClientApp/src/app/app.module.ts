import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { HTTP_INTERCEPTORS, provideHttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { SignInComponent } from './components/sign-in/sign-in.component';
import { HomeComponent } from './pages/home/home.component';
import { ShortUrlTable } from './pages/short-urls-table/short-urls-table.component';
import { AboutPage } from './pages/about/about.component';
import { UrlInfo } from './pages/url-info/url-info.component';

import { AuthGuard } from './guards/auth.guard';
import { AuthInterceptor } from './services/token.interceptor';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    SignInComponent,
    AboutPage,
    ShortUrlTable,
    UrlInfo,
  ],
  imports: [
    BrowserModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'about', component: AboutPage, pathMatch: 'prefix' },
      {
        path: 'short-urls-table',
        component: ShortUrlTable,
        pathMatch: 'prefix',
      },
      { path: 'info/:id', component: UrlInfo, canActivate: [AuthGuard] },
    ]),
  ],
  providers: [
    provideHttpClient(),
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
