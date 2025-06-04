import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from './navbar/navbar.component';
import { LoadingComponent } from './loading/loading.component';
import { NotFoundComponent } from './not-found/not-found.component';



@NgModule({
  declarations: [
    NavbarComponent,
    LoadingComponent,
    NotFoundComponent
  ],
  imports: [
    CommonModule
  ]
})
export class SharedModule { }
