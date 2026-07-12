import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-task-list',
  imports: [],
  templateUrl: './task-list.html',
})
export class TaskList {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly user = this.auth.currentUser;

  constructor() {
    // Populate the header with the authenticated user's profile.
    this.auth.loadCurrentUser().subscribe({ error: () => {} });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
