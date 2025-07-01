import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private isDark = false;

  constructor() {
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'dark') {
      this.setDarkTheme();
    } else {
      this.setLightTheme();
    }
  }

  toggleTheme() {
    this.isDark = !this.isDark;
    if (this.isDark) {
      this.setDarkTheme();
    } else {
      this.setLightTheme();
    }
  }

  setDarkTheme() {
    document.body.classList.add('dark-theme');
    document.body.classList.remove('light-theme');
    localStorage.setItem('theme', 'dark');
    this.isDark = true;
  }

  setLightTheme() {
    document.body.classList.add('light-theme');
    document.body.classList.remove('dark-theme');
    localStorage.setItem('theme', 'light');
    this.isDark = false;
  }

  isDarkMode() {
    return this.isDark;
  }
}
