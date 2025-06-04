import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from './navbar/navbar.component';
import { LoadingComponent } from './loading/loading.component';
import { RouterModule } from '@angular/router';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

const MATERIAL_MODULES = [
  MatInputModule,
  MatButtonModule,
  MatCardModule,
  MatIconModule,
  MatProgressSpinnerModule,
  MatInputModule,
  MatToolbarModule,
  MatButtonModule,
];

@NgModule({
  declarations: [
    NavbarComponent,
    LoadingComponent
  ],
  imports: [
    CommonModule,
    RouterModule,
    ...MATERIAL_MODULES
  ],
  exports: [
    LoadingComponent,
    NavbarComponent,
    CommonModule,
    ...MATERIAL_MODULES
  ]
})
export class SharedModule { }
