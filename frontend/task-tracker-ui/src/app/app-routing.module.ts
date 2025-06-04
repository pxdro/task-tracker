import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

const routes: Routes = [
  { path: '', redirectTo: 'tasks', pathMatch: 'full' },
  {
    path: 'tasks',
    loadChildren: () =>
      import('./tasks/tasks.module').then((m) => m.TasksModule),
    canActivate: [authGuard],
  }, // LazyLoading + AuthGuard
  { 
    path: 'auth', 
    loadChildren: () => 
      import('./auth/auth.module').then(m => m.AuthModule) 
  }, // LazyLoading
  { path: '**', redirectTo: 'tasks' }];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
