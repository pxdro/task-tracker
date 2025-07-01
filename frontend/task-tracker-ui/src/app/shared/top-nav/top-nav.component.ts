import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { ThemeService } from 'src/app/core/services/theme.service';
import { AuthService } from 'src/app/core/services/auth.service';
import { jwtDecode } from 'jwt-decode';


@Component({
  selector: 'app-top-nav',
  templateUrl: './top-nav.component.html',
  styleUrls: ['./top-nav.component.scss']
})
export class TopNavComponent {
  userEmail: string | null = null;

  constructor(
    public themeService: ThemeService,
    private authService: AuthService,
    private router: Router
  ) {
    this.loadUserEmail();
  }

  loadUserEmail() {
    const token = this.authService.getAuthToken();
    if (token) {
      try {
        const decoded: any = jwtDecode(token);
        this.userEmail = decoded?.email || null;
      } catch {
        this.userEmail = null;
      }
    }
  }

  toggleTheme() {
    this.themeService.toggleTheme();
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }
}
