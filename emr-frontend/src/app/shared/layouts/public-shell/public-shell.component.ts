import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-public-shell',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './public-shell.component.html',
  styleUrl: './public-shell.component.scss'
})
export class PublicShellComponent {
  currentYear = new Date().getFullYear();
}
