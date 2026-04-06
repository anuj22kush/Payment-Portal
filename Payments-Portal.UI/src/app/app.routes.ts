import { Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { PaymentFormComponent } from './payment-form/payment-form.component';
import { PaymentsResolver } from './services/payments.resolver';

export const routes: Routes = [
    { path: '', component: DashboardComponent, resolve: { payments: PaymentsResolver } },
    { path: 'new', component: PaymentFormComponent },
    { path: 'edit/:id', component: PaymentFormComponent },
    { path: '**', redirectTo: '' }
];
