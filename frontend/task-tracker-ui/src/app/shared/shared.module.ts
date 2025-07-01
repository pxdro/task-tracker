import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TopNavComponent } from './top-nav/top-nav.component';

const MATERIAL_MODULES = [
  MatInputModule,
  MatButtonModule,
  MatCardModule,
  MatIconModule,
  MatToolbarModule,
  MatTooltipModule,
  MatProgressSpinnerModule
];

@NgModule({
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    ...MATERIAL_MODULES
  ],
  exports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    TopNavComponent,
    ...MATERIAL_MODULES
  ],
  declarations: [
    TopNavComponent
  ]
})
export class SharedModule { }
