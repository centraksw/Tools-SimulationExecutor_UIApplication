import { NgModule } from '@angular/core';
import { ServerModule } from '@angular/platform-server';
import { HttpClientModule, provideHttpClient, withFetch } from '@angular/common/http';

import { AppModule } from './app.module';
import { AppComponent } from './app.component';

@NgModule({
  imports: [
    AppModule,
    ServerModule,
    HttpClientModule
  ],
  providers: [
    provideHttpClient(
      withFetch() // Enable fetch API
    )
  ],
  bootstrap: [AppComponent],
})
export class AppServerModule {}
