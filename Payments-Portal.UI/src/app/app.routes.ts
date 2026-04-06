import { Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { PaymentFormComponent } from './payment-form/payment-form.component';

export const routes: Routes = [
    { path: '', component: DashboardComponent },
    { path: 'new', component: PaymentFormComponent },
    { path: 'edit/:id', component: PaymentFormComponent },
    { path: '**', redirectTo: '' }
];
