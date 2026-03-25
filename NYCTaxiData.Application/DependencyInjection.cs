//using FluentValidation;
//using MediatR;
//using Microsoft.Extensions.DependencyInjection;
//using NYCTaxiData.Application.Behaviors;
//using System.Reflection;

//namespace SFIP.Application
//{
//    public static class DependencyInjection
//    {
//        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
//        {
//            // 1. تسجيل كل الـ Validators (شروط التحقق) الموجودة في هذا المشروع تلقائياً
//            // هذا السطر سيبحث عن أي كلاس يرث من AbstractValidator ويسجله.
//            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

//            // 2. تسجيل MediatR والـ Pipeline Behaviors
//            services.AddMediatR(config =>
//            {
//                // أ. تسجيل كل الـ Handlers والـ Commands/Queries تلقائياً
//                config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

//                // ب. تسجيل الـ Pipeline Behaviors (الترتيب هنا هو سر المهنة - Order is STRICTLY important!)

//                // الغلاف الخارجي: تسجيل العمليات وقياس وقت الأداء
//                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
//                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(MetricsAndPerformanceBehavior<,>));

//                // الطبقة الثانية: الكاش (إذا كانت الداتا في الذاكرة، نرجعها فوراً ولن نكمل باقي الخطوات)
//                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

//                // الطبقة الثالثة: التحقق (موظف الأمن - يرفض الطلب إذا كانت الداتا غير صالحة)
//                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

//                // الطبقة الرابعة: منع التكرار (يحمي النظام من تنفيذ نفس الطلب مرتين في نفس اللحظة)
//                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));

//                // الغلاف الداخلي (قبل الـ Handler مباشرة): فتح وإغلاق الـ Database Transaction
//                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
//            });

//            return services;
//        }
//    }
//}