import { Component, inject, signal } from '@angular/core';
import { Router, RouterOutlet } from "@angular/router";
import { AuthService } from '@services/auth.service';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  isLoggedIn = signal<boolean>(true);

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
